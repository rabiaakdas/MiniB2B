import { useState } from 'react';
import type { Banner } from '../types/shop';
import { MediaImage } from './MediaImage';

interface BannerCarouselProps {
  banners: Banner[];
}

export function BannerCarousel({ banners }: BannerCarouselProps) {
  const [activeIndex, setActiveIndex] = useState(0);

  if (banners.length === 0) {
    return null;
  }

  const banner = banners[Math.min(activeIndex, banners.length - 1)];
  const hasMultiple = banners.length > 1;

  function goToPrevious() {
    setActiveIndex((current) => (current === 0 ? banners.length - 1 : current - 1));
  }

  function goToNext() {
    setActiveIndex((current) => (current === banners.length - 1 ? 0 : current + 1));
  }

  const content = (
    <MediaImage path={banner.imagePath} alt={banner.title} className="banner-image" />
  );

  return (
    <section className="banner-carousel" aria-label="Ana sayfa bannerları">
      <div className="banner-slide">{content}</div>

      {hasMultiple && (
        <div className="banner-controls" aria-label="Banner kontrolleri">
          <button type="button" className="secondary-button icon-button" onClick={goToPrevious} aria-label="Önceki banner">
            ‹
          </button>
          <div className="banner-dots" aria-label="Banner göstergeleri">
            {banners.map((item, index) => (
              <button
                key={item.id}
                type="button"
                className={index === activeIndex ? 'dot active' : 'dot'}
                onClick={() => setActiveIndex(index)}
                aria-label={`${index + 1}. banner`}
                aria-current={index === activeIndex}
              />
            ))}
          </div>
          <button type="button" className="secondary-button icon-button" onClick={goToNext} aria-label="Sonraki banner">
            ›
          </button>
        </div>
      )}
    </section>
  );
}
