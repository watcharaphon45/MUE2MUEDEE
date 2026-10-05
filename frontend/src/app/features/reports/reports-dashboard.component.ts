import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ReportsService } from './reports.service';
import { AuthService } from '../../core/auth/auth.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { SalesReportQuery } from './reports.models';

type ReportTab = 'sales' | 'aging';
type PresetPeriod = 'today' | 'week' | 'month' | 'last30' | 'custom';

@Component({
  selector: 'app-reports-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, StatusBadgeComponent],
  templateUrl: './reports-dashboard.component.html',
  styleUrl: './reports-dashboard.component.css'
})
export class ReportsDashboardComponent implements OnInit {
  protected readonly reportsService = inject(ReportsService);
  protected readonly authService = inject(AuthService);

  readonly activeTab = signal<ReportTab>('sales');
  readonly selectedPeriod = signal<PresetPeriod>('month');

  // Custom date filters
  startDateInput = '';
  endDateInput = '';

  // Aging filter
  readonly selectedAgingBucket = signal<string>('all');

  ngOnInit(): void {
    this.applyPeriod('month');
    this.reportsService.loadStockAgingReport().subscribe();
  }

  setTab(tab: ReportTab): void {
    this.activeTab.set(tab);
    if (tab === 'aging') {
      this.reportsService.loadStockAgingReport().subscribe();
    } else {
      this.refreshSales();
    }
  }

  applyPeriod(period: PresetPeriod): void {
    this.selectedPeriod.set(period);
    const now = new Date();

    if (period === 'today') {
      const todayStr = now.toISOString().slice(0, 10);
      this.startDateInput = todayStr;
      this.endDateInput = todayStr;
    } else if (period === 'week') {
      const sevenDaysAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
      this.startDateInput = sevenDaysAgo.toISOString().slice(0, 10);
      this.endDateInput = now.toISOString().slice(0, 10);
    } else if (period === 'month') {
      const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
      this.startDateInput = firstDay.toISOString().slice(0, 10);
      this.endDateInput = now.toISOString().slice(0, 10);
    } else if (period === 'last30') {
      const thirtyDaysAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
      this.startDateInput = thirtyDaysAgo.toISOString().slice(0, 10);
      this.endDateInput = now.toISOString().slice(0, 10);
    }

    this.refreshSales();
  }

  onCustomDateChange(): void {
    this.selectedPeriod.set('custom');
    this.refreshSales();
  }

  refreshSales(): void {
    const query: SalesReportQuery = {};
    if (this.startDateInput) {
      query.startDate = new Date(this.startDateInput + 'T00:00:00Z').toISOString();
    }
    if (this.endDateInput) {
      query.endDate = new Date(this.endDateInput + 'T23:59:59Z').toISOString();
    }
    this.reportsService.loadSalesReport(query).subscribe();
  }

  exportSalesCsv(): void {
    const query: SalesReportQuery = {};
    if (this.startDateInput) {
      query.startDate = new Date(this.startDateInput + 'T00:00:00Z').toISOString();
    }
    if (this.endDateInput) {
      query.endDate = new Date(this.endDateInput + 'T23:59:59Z').toISOString();
    }
    this.reportsService.downloadSalesReportCsv(query).subscribe();
  }

  exportAgingCsv(): void {
    this.reportsService.downloadStockAgingCsv().subscribe();
  }

  getFilteredAgingItems() {
    const report = this.reportsService.stockAgingReport();
    if (!report) return [];
    const bucket = this.selectedAgingBucket();
    if (bucket === 'all') return report.agingItems;
    if (bucket === 'over60') return report.agingItems.filter(i => i.daysInStock >= 61);
    if (bucket === 'over90') return report.agingItems.filter(i => i.daysInStock >= 91);
    return report.agingItems;
  }
}
