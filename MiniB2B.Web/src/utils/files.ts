const maxImageSize = 5 * 1024 * 1024;
const allowedImageTypes = ['image/jpeg', 'image/png', 'image/webp'];

export function validateImageFile(file: File | null): string | null {
  if (!file) {
    return null;
  }

  if (file.size > maxImageSize) {
    return 'Dosya boyutu en fazla 5 MB olabilir.';
  }

  if (!allowedImageTypes.includes(file.type)) {
    return 'Sadece JPG, PNG veya WEBP görseller kabul edilir.';
  }

  return null;
}
