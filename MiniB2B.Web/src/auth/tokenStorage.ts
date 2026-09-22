const tokenKey = 'miniB2B_token';

export const tokenStorage = {
  get(): string | null {
    return localStorage.getItem(tokenKey);
  },

  set(token: string): void {
    localStorage.setItem(tokenKey, token);
  },

  clear(): void {
    localStorage.removeItem(tokenKey);
  },
};
