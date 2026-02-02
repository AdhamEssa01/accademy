export interface PagedRequest {
  page: number;
  pageSize: number;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
}
