export interface Transaction {
  id: number;
  bankAccountId: number;
  amount: number;
  date: string;        // ISO-String vom Backend
  type: string;
  description: string;
  category: string;
  accountHolder: string;
}
