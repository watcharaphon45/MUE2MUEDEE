import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { defer, finalize } from 'rxjs';
import { LoadingService } from '../loading/loading.service';

export const loadingInterceptor: HttpInterceptorFn = (request, next) => {
  const loading = inject(LoadingService);
  return defer(() => {
    const finish = loading.begin();
    return defer(() => next(request)).pipe(finalize(finish));
  });
};
