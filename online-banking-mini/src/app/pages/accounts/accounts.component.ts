import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountsService } from '../../services/accounts.service';
import { TransactionsChartComponent } from '../../components/transactions-chart/transactions-chart.component';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-accounts',
  standalone: true,
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss',
  imports: [CommonModule, TransactionsChartComponent]
})
export class AccountsComponent implements OnInit {
  accounts: any[] = [];

  constructor(
    private accountsService: AccountsService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.accountsService.getUserAccounts().subscribe({
      next: (response: any) => {
        // response kann ein einzelnes Objekt ODER ein Array sein.
        const list = Array.isArray(response)
          ? response
          : (response ? [response] : []);

        console.log('Accounts from API (normalisiert)', list);
        this.accounts = list;
      },
      error: err => console.error('Fehler beim Laden der Konten', err)
    });
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
