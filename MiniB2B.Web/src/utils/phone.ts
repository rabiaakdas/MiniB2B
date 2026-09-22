export const phoneValidationMessage = 'Telefon numarası 05 ile başlamalı ve 11 haneli olmalıdır.';

export function digitsOnly(value: string): string {
  return value.replace(/\D/g, '').slice(0, 11);
}

export function isValidOptionalTurkishPhone(value: string | null | undefined): boolean {
  if (!value) {
    return true;
  }

  return /^05\d{9}$/.test(value);
}
