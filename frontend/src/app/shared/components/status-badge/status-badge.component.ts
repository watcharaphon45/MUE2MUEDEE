import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.css'
})
export class StatusBadgeComponent implements OnChanges {
  @Input() status: string = '';
  @Input() customLabel?: string;

  badgeClass: string = 'bg-slate-100 text-slate-600 border-slate-300';
  dotClass: string = 'bg-slate-400';
  displayLabel: string = '';

  ngOnChanges(): void {
    const key = (this.status || '').toUpperCase().trim();
    this.displayLabel = this.customLabel || this.resolveLabel(key);

    switch (key) {
      case 'AVAILABLE':
        this.badgeClass = 'bg-emerald-500/15 text-emerald-700 border-emerald-500/30';
        this.dotClass = 'bg-emerald-400';
        break;

      case 'PENDING_INSPECTION':
      case 'WAITING':
        this.badgeClass = 'bg-amber-500/15 text-amber-700 border-amber-500/30';
        this.dotClass = 'bg-amber-400';
        break;

      case 'RESERVED':
        this.badgeClass = 'bg-sky-500/15 text-sky-700 border-sky-500/30';
        this.dotClass = 'bg-sky-400';
        break;

      case 'SOLD':
        this.badgeClass = 'bg-slate-100 text-slate-500 border-slate-300';
        this.dotClass = 'bg-slate-500';
        break;

      case 'REPAIR':
      case 'IN_REPAIR':
        this.badgeClass = 'bg-blue-500/15 text-blue-700 border-blue-500/30';
        this.dotClass = 'bg-blue-400';
        break;

      case 'RETURNED':
      case 'DAMAGED':
        this.badgeClass = 'bg-rose-500/15 text-rose-700 border-rose-500/30';
        this.dotClass = 'bg-rose-400';
        break;

      case 'OWNER':
        this.badgeClass = 'bg-amber-500/20 text-amber-700 border-amber-500/40';
        this.dotClass = 'bg-amber-400';
        break;

      case 'ADMIN':
        this.badgeClass = 'bg-blue-500/20 text-blue-700 border-blue-500/40';
        this.dotClass = 'bg-blue-400';
        break;

      case 'STAFF':
        this.badgeClass = 'bg-slate-100 text-slate-600 border-slate-300';
        this.dotClass = 'bg-slate-400';
        break;

      case 'ACTIVE':
        this.badgeClass = 'bg-emerald-500/15 text-emerald-700 border-emerald-500/30';
        this.dotClass = 'bg-emerald-400';
        break;

      case 'INACTIVE':
        this.badgeClass = 'bg-rose-500/15 text-rose-700 border-rose-500/30';
        this.dotClass = 'bg-rose-400';
        break;

      default:
        this.badgeClass = 'bg-slate-100 text-slate-600 border-slate-300';
        this.dotClass = 'bg-slate-400';
        break;
    }
  }

  private resolveLabel(key: string): string {
    const labels: Record<string, string> = {
      'AVAILABLE': 'พร้อมขาย',
      'PENDING_INSPECTION': 'รอตรวจสภาพ',
      'RESERVED': 'จองแล้ว',
      'SOLD': 'ขายแล้ว',
      'REPAIR': 'ส่งซ่อม',
      'RETURNED': 'คืนสินค้า',
      'DAMAGED': 'ชำรุด',
      'OWNER': 'เจ้าของ (Owner)',
      'ADMIN': 'ผู้จัดการ (Admin)',
      'STAFF': 'พนักงาน (Staff)',
      'ACTIVE': 'ใช้งานปกติ',
      'INACTIVE': 'ระงับการใช้งาน'
    };
    return labels[key] || this.status;
  }
}
