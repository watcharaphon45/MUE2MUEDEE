import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { SelectComponent, SelectOption } from '../shared/components';

export interface NavItem {
  label: string;
  route: string;
  icon: string;
  exact?: boolean;
}

export interface NavGroup {
  title: string;
  items: NavItem[];
}

const SIDEBAR_STORAGE_KEY = 'mue2muedee_sidebar_collapsed';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, SelectComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent {
  protected readonly authService = inject(AuthService);

  readonly isSidebarCollapsed = signal<boolean>(
    typeof window !== 'undefined' && localStorage.getItem(SIDEBAR_STORAGE_KEY) === 'true'
  );

  readonly isMobileMenuOpen = signal<boolean>(false);

  toggleSidebar(): void {
    const nextState = !this.isSidebarCollapsed();
    this.isSidebarCollapsed.set(nextState);
    try {
      localStorage.setItem(SIDEBAR_STORAGE_KEY, String(nextState));
    } catch {
      // Ignore storage errors in restricted environments
    }
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update(v => !v);
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }

  readonly menuGroups = computed<NavGroup[]>(() => {
    const groups: NavGroup[] = [
      {
        title: 'ภาพรวมระบบ',
        items: [
          { label: 'Dashboard ภาพรวม', route: '/dashboard', icon: 'dashboard' }
        ]
      },
      {
        title: 'งานบริการหน้าร้าน',
        items: [
          { label: 'POS ขายหน้าร้าน', route: '/sales/pos', icon: 'point_of_sale' },
          { label: 'รับซื้อ & ตรวจสภาพ (PO)', route: '/purchase', icon: 'shopping_bag', exact: false },
          { label: 'ประวัติการขาย (Sales)', route: '/sales', icon: 'receipt_long', exact: true }
        ]
      },
      {
        title: 'คลังสินค้า & Serial',
        items: [
          { label: 'คลังสินค้า (Inventory)', route: '/inventory', icon: 'inventory_2', exact: false },
          { label: 'สืบค้น Serial Trace', route: '/sales/trace', icon: 'manage_search' }
        ]
      },
      {
        title: 'ข้อมูลหลัก & ทะเบียน',
        items: [
          { label: 'ลูกค้า & ผู้ขายสินค้า', route: '/customers', icon: 'contacts' },
          { label: 'ข้อมูลมาสเตอร์ (Master Data)', route: '/master-data', icon: 'dataset' }
        ]
      },
      {
        title: 'รายงาน & ตั้งค่าระบบ',
        items: [
          { label: 'รายงาน & สถิติ', route: '/reports', icon: 'trending_up' },
          { label: 'ข้อมูลร้านค้า & ผู้ใช้งาน', route: '/settings', icon: 'storefront' }
        ]
      }
    ];

    if (this.authService.isSuperAdmin()) {
      groups.push({
        title: 'ผู้ดูแลระบบส่วนกลาง (Super Admin)',
        items: [
          { label: 'จัดการร้านค้า & เจ้าของร้าน', route: '/super-admin/stores', icon: 'admin_panel_settings' }
        ]
      });
    }

    return groups;
  });

  readonly storeOptions = computed<SelectOption[]>(() =>
    this.authService.availableStores().map(s => ({
      value: s.storeId,
      label: `${s.storeName} (${s.role})`
    }))
  );

  onStoreSelect(storeId: any): void {
    if (storeId && storeId !== this.authService.currentStore()?.storeId) {
      this.authService.switchStore(storeId).subscribe();
    }
  }
}

