import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { RadioButtonModule } from 'primeng/radiobutton';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';
import { FileUploadModule } from 'primeng/fileupload';
import { RatingModule } from 'primeng/rating';
import { RippleModule } from 'primeng/ripple';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { ProgressBarModule } from 'primeng/progressbar';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TabViewModule } from 'primeng/tabview';
import { PanelModule } from 'primeng/panel';
import { DividerModule } from 'primeng/divider';
import { MenubarModule } from 'primeng/menubar';
import { BadgeModule } from 'primeng/badge';
import { AvatarModule } from 'primeng/avatar';
import { InputNumberModule } from 'primeng/inputnumber';
import { TooltipModule } from 'primeng/tooltip';

// Components
import { AppComponent } from './app.component';
import { AuthorizationComponent } from './components/authorization.component';
import { InventoryComponent } from './components/inventory.component';
import { HRDashboardComponent } from './components/hr-dashboard.component';
import { InventoryDashboardComponent } from './components/inventory-dashboard.component';
import { SupplierManagementComponent } from './components/supplier-management.component';


// Services
import { ApiService } from './services/api.service';

// PrimeNG Services
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: AppComponent },
  { path: 'authorization', component: AuthorizationComponent },
  { path: 'hr-dashboard', component: HRDashboardComponent },
  { path: 'suppliers', component: SupplierManagementComponent },
  { path: 'inventory', component: InventoryComponent },
  { path: 'inventory-dashboard', component: InventoryDashboardComponent },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  declarations: [
    AppComponent,
    AuthorizationComponent,
    InventoryComponent,
    HRDashboardComponent,
    InventoryDashboardComponent,
    SupplierManagementComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot(routes),
    
    // PrimeNG Modules
    ButtonModule,
    CardModule,
    TableModule,
    InputTextModule,
    InputTextareaModule,
    DropdownModule,
    CalendarModule,
    CheckboxModule,
    RadioButtonModule,
    DialogModule,
    ConfirmDialogModule,
    ToastModule,
    ToolbarModule,
    FileUploadModule,
    RatingModule,
    RippleModule,
    TagModule,
    ChipModule,
    ProgressBarModule,
    ProgressSpinnerModule,
    TabViewModule,
    PanelModule,
    DividerModule,
    MenubarModule,
    BadgeModule,
    AvatarModule,
    InputNumberModule,
    TooltipModule
  ],
  providers: [
    ApiService,
    MessageService,
    ConfirmationService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
