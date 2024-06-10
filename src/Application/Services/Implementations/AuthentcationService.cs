using Application.Services.Interfaces;
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Services;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Implementations;

public class AuthentcationService(IUnitOfWork unitOfWork, FilesUploader filesUploader, JWTHandler jWTHandler)
    : IAuthentcationService
{
    public async Task<List<string>> Register(User user, string password, string role)
    {
        if (unitOfWork.UserRepository.GetByFilter(u => u.Email == user.Email).FirstOrDefault() != null)
        {
            return ["Sorry, this email has already been registered. Please use a different email address.", "400"];
        }
        if (unitOfWork.UserRepository.GetByFilter(u => u.SSN == user.SSN).FirstOrDefault() != null)
        {
            return ["Sorry, SSN has already been used. ", "400"];
        }
        if (unitOfWork.UserRepository.GetByFilter(u => u.UserName == user.UserName).FirstOrDefault() != null)
        {
            return ["Sorry, UserName Is already existing", "400"];
        }
        var result = await unitOfWork.UserRepository.CreateUserAsync(user, password);
        if (!result.Succeeded)
        {
            return ["Sorry, Failed Try Again", "400"];
        }
        var result1 = await unitOfWork.UserRepository.AddToRoleAsync(user, role);
        if (!result1.Succeeded)
        {
            await unitOfWork.UserRepository.DeleteAsync(user);
            return ["Sorry, Failed Try Again", "400"];
        }
        if (user.Picture != null)
        {
            var errors = filesUploader.ValidateFile(user.Picture);
            if (errors.IsNullOrEmpty())
            {
                user.FilePath = await filesUploader.UploadPictureAsync(user.Picture);
                unitOfWork.UserRepository.Update(user);
            }
            else
            {
                await unitOfWork.UserRepository.DeleteAsync(user);
                return ["Sorry, Failed To Upload Picture", "400"];
            }
        }
        if (role == "Student")
        {
            var services = unitOfWork.ServiceRepository.GetAll();
            foreach (var service in services)
            {
                if (service.CollegeName != user.College_Name
                    || service.SquadYear != user.Squad_Year) continue;
                user.services ??= [];
                user.services.Add(service);
            }
        }
        await unitOfWork.SaveAsync();
        var verificationCode = _generateVerificationCode();
        _saveVerificationCodeInDatabase(user.Email, verificationCode);
        await EmailSender.SendVerificationCodeToEmailAsync(user.Email, verificationCode);
        return [$"Send Verification Code Successfly ", user.Id, "200"];

    }


    public async Task<List<string>> LogIn(string email, string password)
    {
        var user = unitOfWork.UserRepository.GetByFilter(u => u.Email == email).FirstOrDefault();
        if (user is null)
        {
            return
            [
                "Invalid email or password combination. Please check your credentials and try again.", "400"
            ];
        }
        if (user.IsStoped)
        {
            return ["Your Account is Stopped ", "400"];
        }
        var result = await unitOfWork.UserRepository.CheckPasswordAsync(user, password);

        if (!result)
        {
            return
            [
                "Invalid email or password combination. Please check your credentials and try again.", "400"
            ];
        }

        if (user.EmailConfirmed != false)
            return
            [
                "LogIn Done Successfly", await jWTHandler.GenerateToken(user),
                    unitOfWork.UserRepository.GetRolesAsync(user).Result[0], "200"
            ];
        var verificationCode = _generateVerificationCode();
        _saveVerificationCodeInDatabase(user.Email, verificationCode);
        await EmailSender.SendVerificationCodeToEmailAsync(user.Email, verificationCode);
        return ["Please verify your email first Send Verification Code Again Successfly ", user.Id, "400"];
    }


    public async Task<List<string>> VerifyEmail(string userId, int VerificationCode)
    {
        var user = unitOfWork.UserRepository.GetByFilter(u => u.Id == userId).FirstOrDefault();
        if (user is null)
            return ["Please Register Again", "400"];
        if (user.VerificationCode != VerificationCode)
            return ["Invalid Verification Code", "400"];
        user.VerificationCode = null;
        user.EmailConfirmed = true;
        await unitOfWork.SaveAsync();
        return ["Verified", "200"];
    }

    public async Task<List<string>> ResendCodeToEmail(string userId)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        var verificationCode = _generateVerificationCode();
        _saveVerificationCodeInDatabase(user.Email, verificationCode);
        await EmailSender.SendVerificationCodeToEmailAsync(user.Email, verificationCode);
        return ["Resend Verification Code Again Successfly", "200"];
    }

    public async Task<List<string>> SendCodeToEmailToResetPassword(string email)
    {
        var user = unitOfWork.UserRepository.GetByFilter(u => u.Email == email).FirstOrDefault();
        if (user is null)
        {
            return ["Register First", "400"];
        }
        var verificationCode = _generateVerificationCode();
        _saveVerificationCodeInDatabase(user.Email, verificationCode);
        await EmailSender.SendVerificationCodeToEmailAsync(user.Email, verificationCode);
        user.EmailConfirmed = false;
        await unitOfWork.SaveAsync();
        return ["Send Verification Code Successfly ", user.Id, "200"];
    }



    public async Task<List<string>> ResetPassword(string userId, string newPassword)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return ["Please Register First", "400"];
        }
        if (user.EmailConfirmed == false)
        {
            return ["Please verify your email first", user.Id, "400"];
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await unitOfWork.UserRepository.RemovePasswordAsync(user);
            if (!result.Succeeded)
            {
                return ["Failed Try Again", "400"];
            }

            result = await unitOfWork.UserRepository.AddPasswordAsync(user, newPassword);
            if (!result.Succeeded)
            {
                return ["Failed Try Again", "400"];
            }

            await unitOfWork.SaveAsync();
            await transaction.CommitAsync();
            return ["Update Password Successflu", "200"];
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return [e.Message, "500"];
        }
    }


    #region HelperMethods

    private int _generateVerificationCode()
    {
        var rnd = new Random();
        return rnd.Next(100000, 999999);
    }


    private void _saveVerificationCodeInDatabase(string email, int verificationCode)
    {
        var user = unitOfWork.UserRepository.GetByFilter(u => u.Email == email).FirstOrDefault();
        if (user != null)
        {
            user.VerificationCode = verificationCode;
            unitOfWork.SaveAsync();
        }
    }
}
#endregion



