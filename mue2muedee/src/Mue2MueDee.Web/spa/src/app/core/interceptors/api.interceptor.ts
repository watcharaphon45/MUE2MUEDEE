import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('/api')) {
    const newUrl = req.url.replace(/^\/api/, environment.apiUrl);
    const apiReq = req.clone({ url: newUrl });
    return next(apiReq);
  }
  return next(req);
};
