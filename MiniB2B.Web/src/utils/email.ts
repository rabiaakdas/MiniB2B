export const emailValidationMessage = 'Geçerli bir e-posta adresi giriniz.';

export function isValidEmail(value: string): boolean {
  return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(value.trim());
}
