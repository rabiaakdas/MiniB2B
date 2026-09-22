import { useState } from 'react';
import { resolveMediaUrl } from '../utils/media';

interface MediaImageProps {
  path: string | null | undefined;
  alt: string;
  className?: string;
}

export function MediaImage({ path, alt, className }: MediaImageProps) {
  const [hasError, setHasError] = useState(false);
  const url = resolveMediaUrl(path);

  if (!url || hasError) {
    return (
      <div className={`media-placeholder ${className ?? ''}`} aria-label={alt || 'Görsel yok'}>
        Görsel yok
      </div>
    );
  }

  return (
    <img
      className={className}
      src={url}
      alt={alt}
      loading="lazy"
      onError={() => setHasError(true)}
    />
  );
}
