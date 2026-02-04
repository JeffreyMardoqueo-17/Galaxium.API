using FluentValidation;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Entities;
namespace Galaxium.Api.Validators
{
    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator(ICustomerRepository customerRepository)
        {
            // Nombre completo
            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("El nombre completo es obligatorio.")
                .MaximumLength(150)
                .WithMessage("El nombre no puede superar los 150 caracteres.");

            // Email (obligatorio y único)
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress()
                .WithMessage("El formato del correo no es válido.")
                .MustAsync(async (email, _) =>
                    !await customerRepository.ExistsByEmailAsync(email!))
                .WithMessage("Este correo ya está registrado.");

            // Teléfono (opcional y repetible)
            RuleFor(x => x.Phone)
                .Matches(@"^[0-9]{8,15}$")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("El teléfono debe contener solo números (8 a 15 dígitos).");
        }
    }
}
