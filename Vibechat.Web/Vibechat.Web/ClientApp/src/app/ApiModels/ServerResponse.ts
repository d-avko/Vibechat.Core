export class ServerResponse<T> {
  isSuccessfull: boolean;
  errorMessage: string;
  response: T
}
