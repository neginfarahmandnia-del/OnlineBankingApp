export interface BankAccount {
  id: number;
  iban: string;
  name: string;
  accountHolder: string;

  // diese Felder kommen laut Network-Response ebenfalls vom Backend:
  balance: number;
  warnLimit: number;
  kontotyp: string;
  abteilung: string;
}
