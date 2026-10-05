import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login.component';
import { MainLayoutComponent } from './layout/main-layout.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { StoreSettingsComponent } from './features/store/store-settings.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        component: DashboardComponent
      },
      {
        path: 'inventory',
        children: [
          {
            path: '',
            loadComponent: () => import('./features/inventory/inventory-list/inventory-list.component').then(m => m.InventoryListComponent)
          },
          {
            path: 'new',
            loadComponent: () => import('./features/inventory/inventory-form/inventory-form.component').then(m => m.InventoryFormComponent)
          },
          {
            path: ':id',
            loadComponent: () => import('./features/inventory/inventory-detail/inventory-detail.component').then(m => m.InventoryDetailComponent)
          }
        ]
      },
      {
        path: 'products',
        loadComponent: () => import('./features/master-data/products/product-list.component').then(m => m.ProductListComponent)
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/master-data/customers/customer-list.component').then(m => m.CustomerListComponent)
      },
      {
        path: 'settings',
        component: StoreSettingsComponent
      },
      {
        path: 'purchase',
        children: [
          {
            path: '',
            loadComponent: () => import('./features/purchase/purchase-list/purchase-list.component').then(m => m.PurchaseListComponent)
          },
          {
            path: 'new',
            loadComponent: () => import('./features/purchase/purchase-wizard/purchase-wizard.component').then(m => m.PurchaseWizardComponent)
          },
          {
            path: ':id',
            loadComponent: () => import('./features/purchase/purchase-detail/purchase-detail.component').then(m => m.PurchaseDetailComponent)
          }
        ]
      },
      {
        path: 'sales',
        children: [
          {
            path: '',
            loadComponent: () => import('./features/sales/sale-list/sale-list.component').then(m => m.SaleListComponent)
          },
          {
            path: 'pos',
            loadComponent: () => import('./features/sales/pos/pos.component').then(m => m.PosComponent)
          },
          {
            path: 'trace',
            loadComponent: () => import('./features/sales/serial-trace/serial-trace.component').then(m => m.SerialTraceComponent)
          },
          {
            path: ':id',
            loadComponent: () => import('./features/sales/sale-detail/sale-detail.component').then(m => m.SaleDetailComponent)
          }
        ]
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports-dashboard.component').then(m => m.ReportsDashboardComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
