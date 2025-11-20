import {
  Component,
  OnInit,
  OnDestroy,
  ElementRef,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  Chart,
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  Legend
} from 'chart.js';
import { TransactionsService } from '../../services/transactions.service';

Chart.register(
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  Legend
);

@Component({
  selector: 'app-transactions-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './transactions-chart.component.html',
  styleUrl: './transactions-chart.component.scss'
})
export class TransactionsChartComponent implements OnInit, OnDestroy {
  @ViewChild('transactionsChart', { static: true })
  canvasRef!: ElementRef<HTMLCanvasElement>;

  private chart?: Chart<'bar'>;

  constructor(private transactionsService: TransactionsService) { }

  ngOnInit(): void {
    this.transactionsService.getAll().subscribe({
      next: trans => {
        const grouped = new Map<string, number>();

        trans.forEach(t => {
          const cat = t.category || 'Sonstiges';
          grouped.set(cat, (grouped.get(cat) ?? 0) + t.amount);
        });

        const labels = Array.from(grouped.keys());
        const data = Array.from(grouped.values());

        this.renderChart(labels, data);
      },
      error: err => console.error('Fehler beim Laden der Transaktionen', err)
    });
  }

  private renderChart(labels: string[], data: number[]): void {
    // Falls bereits ein Chart existiert: zuerst zerstören
    if (this.chart) {
      this.chart.destroy();
    }

    const ctx = this.canvasRef.nativeElement.getContext('2d');
    if (!ctx) {
      return;
    }

    this.chart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Summe pro Kategorie',
            data
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false
      }
    });
  }

  ngOnDestroy(): void {
    if (this.chart) {
      this.chart.destroy();
    }
  }
}
