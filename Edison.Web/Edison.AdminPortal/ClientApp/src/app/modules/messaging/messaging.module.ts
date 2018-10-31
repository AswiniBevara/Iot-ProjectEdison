import { NgModule } from '@angular/core'
import { CommonModule } from '@angular/common'
import { RecentlyActiveComponent } from './components/recently-active/recently-active.component'
import { MessageListComponent } from './components/message-list/message-list.component'
import { AllUsersChatComponent } from './components/all-users-chat/all-users-chat.component'
import { UserChatComponent } from './components/user-chat/user-chat.component'
import { SharedModule } from '../shared/shared.module'
import { MaterialModule } from '../material/material.module'
import { FormsModule } from '@angular/forms'
import { ScrollingModule } from '@angular/cdk/scrolling';
import { NgxAutoScrollModule } from "ngx-auto-scroll";

@NgModule({
    imports: [ CommonModule, SharedModule, MaterialModule, FormsModule, ScrollingModule, NgxAutoScrollModule ],
    declarations: [
        RecentlyActiveComponent,
        MessageListComponent,
        AllUsersChatComponent,
        UserChatComponent,
    ],
    exports: [ UserChatComponent, AllUsersChatComponent ],
})
export class MessagingModule { }
