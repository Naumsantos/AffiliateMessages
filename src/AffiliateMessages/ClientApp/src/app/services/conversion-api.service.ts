import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, catchError, map, shareReplay, switchMap, throwError } from 'rxjs';
import {
  ConversionResult,
  LocalSession,
} from '../models/conversion-result.model';

@Injectable({ providedIn: 'root' })
export class ConversionApiService {
  private readonly http = inject(HttpClient);
  private sessionRequest?: Observable<string>;

  convert(file: File): Observable<ConversionResult> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.postWithToken<ConversionResult>('/api/conversions', formData);
  }

  openOutputFolder(): Observable<void> {
    return this.postWithToken<void>('/api/output/open-folder', null);
  }

  shutdownHost(): Observable<void> {
    return this.postWithToken<void>('/api/host/shutdown', null);
  }

  private postWithToken<T>(url: string, body: FormData | null): Observable<T> {
    return this.getCsrfToken().pipe(
      switchMap((token) =>
        this.http.post<T>(url, body, {
          headers: new HttpHeaders({ 'X-CSRF-Token': token }),
        }),
      ),
    );
  }

  private getCsrfToken(): Observable<string> {
    if (!this.sessionRequest) {
      this.sessionRequest = this.http
        .get<LocalSession>('/api/session')
        .pipe(
          map((session) => {
            if (typeof session?.csrfToken !== 'string' || session.csrfToken.length === 0) {
              throw new Error('A resposta da sessão local não contém um token CSRF.');
            }
            return session.csrfToken;
          }),
          catchError((error: unknown) => {
            this.sessionRequest = undefined;
            return throwError(() => error);
          }),
          shareReplay({ bufferSize: 1, refCount: false }),
        );
    }

    return this.sessionRequest;
  }
}
