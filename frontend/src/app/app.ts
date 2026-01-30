import { Component, OnInit} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Api, Order } from './api';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div style="padding:16px">
      <h2>Orders</h2>

      <div style="margin-bottom: 20px;">
        <input type="text" [formControl]="orderIdControl" placeholder="Enter Order ID" style="padding: 8px; margin-right: 8px;">
        <button (click)="searchOrderById()" style="padding: 8px 16px;">Search</button>
      </div>

      <div *ngIf="orderNotFound" style="color: red; margin: 12px 0;">
        Order not found
      </div>

      <ng-container *ngIf="orders$ | async as orders; else loading">
        <div *ngFor="let o of orders" style="margin:12px 0; padding:12px; border:1px solid #ddd">
          <div><b>OrderId:</b> {{ o.orderId }}</div>
          <div><b>Customer Name:</b> {{ o.customerName }}</div>
          <div><b>Source System:</b> {{ o.sourceSystem }}</div>
          <div><b>Total Amount:</b> {{ o.totalAmount }}</div>
          <div><b>Order Date:</b> {{ o.orderDate }} </div>
          <div><b>Status:</b> {{ o.status }} </div>
        </div>
      </ng-container>

      <ng-template #loading>
        <div *ngIf="!orderNotFound">Loading...</div>
      </ng-template>
    </div>
  `
})
export class App implements OnInit {
  orders$!: Observable<Order[]>;
  orderIdControl = new FormControl('');
  orderNotFound = false;
  loadingFailed = false;

  constructor(private ordersApi: Api) {}

  ngOnInit(){
    this.orders$ = this.ordersApi.getOrders();
  }

  searchOrderById() {
    const orderId = this.orderIdControl.value;
    this.orderNotFound = false;
    if (!orderId) {
      this.orders$ = this.ordersApi.getOrders().pipe(
        catchError(error => {
          this.loadingFailed = true
          console.log(error)
          return of([]);
        })
      );
      return;
    }
      this.orders$ = this.ordersApi.getOrderById(orderId).pipe(
        map(order => [order]),
        catchError(error => {
          if (error.status === 404) {
            this.orderNotFound = true;
          }
          return of([]);
        })
      );
  }
}


