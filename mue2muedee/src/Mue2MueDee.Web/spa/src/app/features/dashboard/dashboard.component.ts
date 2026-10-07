import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { DashboardService } from './dashboard.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, StatusBadgeComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  protected readonly authService = inject(AuthService);
  protected readonly dashboardService = inject(DashboardService);

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.dashboardService.loadMetrics().subscribe();
  }
}
