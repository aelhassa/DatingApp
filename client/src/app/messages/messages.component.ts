import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/Pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  
  
  pageNumber=1;
  pageSize=2;
  messages:Message[]=[];
  pagination:Pagination;
  container='Outbox';
  loading =false;

  constructor(private messageService :MessageService) { }

  ngOnInit(): void {
    this.loadMessages();
  }
  loadMessages(){
    this.loading=true;
    this.messageService.getMessages(this.pageNumber,this.pageSize,this.container).subscribe(response=>{
      this.messages=response.result; 
      this.pagination=response.pagination;
      this.loading=false;
    })
  }
  deleteMessage(id:number){
    this.loading=true;
    this.messageService.deleteMessage(id).subscribe(()=>{
      this.messages.splice(this.messages.findIndex(m=>m.id===id),1);
      this.loading=false;
    })
  }
  pageChanged(event:any){

    if(this.pageNumber!== event.page){
      this.pageNumber=event.pageNumber;
      this.loadMessages();
    }
  }

}
