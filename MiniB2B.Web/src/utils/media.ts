const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

export function resolveMediaUrl(path: string | null | undefined): string | null {
  if (!path) {
    return null;
  }

  if (path.startsWith('http://') || path.startsWith('https://')) {
    return path;
  }

  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return new URL(normalizedPath, apiBaseUrl).toString();
}
