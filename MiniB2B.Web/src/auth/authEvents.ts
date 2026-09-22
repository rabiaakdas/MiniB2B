type AuthExpiredHandler = () => void;

const handlers = new Set<AuthExpiredHandler>();

export function subscribeToAuthExpired(handler: AuthExpiredHandler): () => void {
  handlers.add(handler);

  return () => {
    handlers.delete(handler);
  };
}

export function notifyAuthExpired(): void {
  handlers.forEach((handler) => handler());
}
