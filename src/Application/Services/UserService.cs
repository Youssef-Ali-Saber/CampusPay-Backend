using Application.DTOs;
using Domain.Entities;
using Domain.IUnitOfWork;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class UserService
    (
    IUnitOfWork unitOfWork,
    FilesUploaderService filesUploader,
    EmailHandlerService emailHandlerService
    )
{
    public async Task<List<string>> Register(UserBaseDto userDto, string password, string role)
    {
        var user = new User
        {
            Email = userDto.Email,
            UserName = userDto.Email,
            FullName = userDto.FullName,
            SSN = userDto.SSN,
            Picture = userDto.Picture
        };

        if (!EmailHandlerService.IsValidDomain(user.Email))
            return ["Invalid Email", "400"];

        if (_checkIsEmailRegisterd(user))
            return ["Sorry, Email has already been used. ", "400"];

        if (_checkIsSSNUsed(user))
            return ["Sorry, SSN has already been used. ", "400"];

        var resultOfCreation = await unitOfWork.UserRepository.CreateUserAsync(user, password);

        if (!resultOfCreation.Succeeded)
            return ["Sorry, Failed Try Again", "400"];

        var resultOfAddingToRole = await unitOfWork.UserRepository.AddToRoleAsync(user, role);
        if (!resultOfAddingToRole.Succeeded)
        {
            await unitOfWork.UserRepository.DeleteAsync(user);
            return ["Sorry, Failed Try Again", "400"];
        }
        if (user.Picture != null)
        {
            var errors = filesUploader.ValidateFile(user.Picture);
            if (errors.IsNullOrEmpty())
            {
                user.FilePath = await filesUploader.UploadImageAsync(user.Picture, "Images/Picture/");

                unitOfWork.UserRepository.Update(user);
            }
            else
            {
                await unitOfWork.UserRepository.DeleteAsync(user);

                return ["Sorry, Failed To Upload Picture", "400"];
            }
        }
        await unitOfWork.SaveAsync();

        await emailHandlerService.SendCodeToEmail(user);

        return [$"Send Verification Code Successfly ", user.Id, "200"];

    }

    private bool _checkIsEmailRegisterd(User user)
    {
        if (unitOfWork.UserRepository.GetByFilter(u => u.Email == user.Email).FirstOrDefault() != null)
        {
            return true;
        }
        return false;
    }
    private bool _checkIsSSNUsed(User user)
    {
        if (unitOfWork.UserRepository.GetByFilter(u => u.SSN == user.SSN).FirstOrDefault() != null)
        {
            return true;
        }
        return false;
    }

}



