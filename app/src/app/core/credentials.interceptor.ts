import { HttpInterceptorFn } from '@angular/common/http';

export const credentialsInterceptor: HttpInterceptorFn = (outgoingRequest, handleNext) => {
  return handleNext(outgoingRequest.clone({ withCredentials: true }));
};
