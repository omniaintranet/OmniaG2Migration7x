export interface SpPageResult<T> {
    __metadata: metadata;
    results: Array<T>;
    __next: string;
}

export interface metadata{
  id: string;
  uri: string;
  etag: string;
  type: string;
}
