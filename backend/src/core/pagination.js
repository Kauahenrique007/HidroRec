function parsePagination(query = {}) {
  const page = Math.max(1, Number.parseInt(query.page, 10) || 1);
  const pageSize = Math.min(100, Math.max(1, Number.parseInt(query.pageSize, 10) || 10));
  const sortBy = query.sortBy || 'updatedAt';
  const order = String(query.order || 'desc').toLowerCase() === 'asc' ? 'asc' : 'desc';

  return { page, pageSize, sortBy, order };
}

function paginate(items, pagination) {
  const total = items.length;
  const totalPages = Math.max(1, Math.ceil(total / pagination.pageSize));
  const safePage = Math.min(pagination.page, totalPages);
  const start = (safePage - 1) * pagination.pageSize;
  const data = items.slice(start, start + pagination.pageSize);

  return {
    data,
    meta: {
      pagination: {
        page: safePage,
        pageSize: pagination.pageSize,
        total,
        totalPages
      }
    }
  };
}

module.exports = { paginate, parsePagination };
