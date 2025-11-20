import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountsService {
  // Basis-URL aus deinem Backend
  private baseUrl = `${environment.apiBaseUrl}/api/Accounts`;

  constructor(private http: HttpClient) { }

  // Backend liefert EIN Konto-Objekt für den eingeloggten Benutzer
  getUserAccounts(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/user-accounts`);
  }
}
