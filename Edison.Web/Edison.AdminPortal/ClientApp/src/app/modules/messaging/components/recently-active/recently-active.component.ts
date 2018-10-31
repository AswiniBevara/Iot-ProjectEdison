import { Component, OnInit } from '@angular/core'
import { Store, select } from '@ngrx/store'
import { AppState } from '../../../../reducers'
import { UpdatePageTitle } from '../../../../reducers/app/app.actions'
import { Observable } from 'rxjs';
import { chatActiveUsersSelector } from '../../../../reducers/chat/chat.selectors';
import { SelectActiveUser, SelectActiveConversation } from '../../../../reducers/chat/chat.actions';

@Component({
    selector: 'app-recently-active',
    templateUrl: './recently-active.component.html',
    styleUrls: [ './recently-active.component.scss' ],
})
export class RecentlyActiveComponent implements OnInit {
    activeUsers$: Observable<any[]>;

    constructor (private store: Store<AppState>) { }

    ngOnInit() {
        this.store.dispatch(new UpdatePageTitle({ title: 'Messaging' }));
        this.activeUsers$ = this.store.pipe(select(chatActiveUsersSelector));
    }

    selectActiveUser(user) {
        this.store.dispatch(new SelectActiveUser({ userId: user.id, userName: user.name }));
        this.store.dispatch(new SelectActiveConversation({ conversationId: user.conversationId }));
    }

    selectAllChat() {
        this.store.dispatch(new SelectActiveUser({ userId: '*' }));
    }
}
