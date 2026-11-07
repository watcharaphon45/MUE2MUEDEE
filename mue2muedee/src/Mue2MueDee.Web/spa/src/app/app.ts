import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LoadingService } from './core/loading/loading.service';
import { GlobalLoadingComponent } from './shared/components/global-loading/global-loading.component';
import { NotificationToastComponent } from './shared/components/notification-toast/notification-toast.component';

@Component({
  standalone: true,
  imports: [RouterOutlet, GlobalLoadingComponent, NotificationToastComponent],
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly loading = inject(LoadingService);
}
