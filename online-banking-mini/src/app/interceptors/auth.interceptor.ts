import { HttpInterceptorFn } from '@angular/common/http';

const TOKEN_KEY = 'ob_jwt'; // muss zu deinem AuthService passen

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem(TOKEN_KEY);

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req);
};
