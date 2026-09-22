interface PaginationProps {
  page: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ page, totalPages, totalCount, pageSize, onPageChange }: PaginationProps) {
  if (totalCount === 0) {
    return null;
  }

  return (
    <div className="pagination" aria-label="Sayfalama">
      <span>
        Sayfa {page} / {Math.max(totalPages, 1)} · {totalCount} ürün
      </span>
      <div>
        <button
          type="button"
          className="secondary-button"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          Önceki
        </button>
        <button
          type="button"
          className="secondary-button"
          disabled={page >= totalPages || totalCount <= pageSize}
          onClick={() => onPageChange(page + 1)}
        >
          Sonraki
        </button>
      </div>
    </div>
  );
}
