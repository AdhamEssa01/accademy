import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from './loading.service';

export const loadingInterceptor: HttpInterceptorFn = (request, next) => {
  const loader = inject(LoadingService);
  loader.show();
  return next(request).pipe(finalize(() => loader.hide()));
};
