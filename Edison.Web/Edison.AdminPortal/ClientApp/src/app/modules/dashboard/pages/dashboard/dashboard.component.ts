import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core'
import { MapDefaults } from '../../../map/models/mapDefaults'
import { MapPin } from '../../../map/models/mapPin'
import { AppState } from '../../../../reducers'
import { Store, select } from '@ngrx/store'
import { MapComponent } from '../../../map/components/map/map.component'
import {
    eventsSelector,
    showEventsSelector,
} from '../../../../reducers/event/event.selectors'
import { GetDevices } from '../../../../reducers/device/device.actions'
import { GetEvents } from '../../../../reducers/event/event.actions'
import { devicesSelector } from '../../../../reducers/device/device.selectors'
import { Event, EventType } from '../../../../reducers/event/event.model'
import {
    responsesExist,
    responsesSelector,
} from '../../../../reducers/response/response.selectors'
import { Observable, combineLatest, Subscription } from 'rxjs'
import {
    ResponseState,
    Response,
} from '../../../../reducers/response/response.model'
import { GetResponses } from '../../../../reducers/response/response.actions'
import { Device } from '../../../../reducers/device/device.model'
import { environment } from '../../../../../environments/environment'
import { Actions } from '@ngrx/effects'
import {
    AppActionTypes,
    UpdatePageTitle,
} from '../../../../reducers/app/app.actions'
import { GetChatAuthToken, AddChat } from '../../../../reducers/chat/chat.actions';
import { chatAuthSelector, chatShowUserChatSelector, chatShowAllChatSelector } from '../../../../reducers/chat/chat.selectors';
import { filter, first } from 'rxjs/operators';
import { DirectlineService } from '../../../../core/services/directline/directline.service';

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: [ './dashboard.component.scss' ]
})
export class DashboardComponent implements OnInit, OnDestroy {
    @ViewChild(MapComponent)
    private mapComponent: MapComponent
    showResponses$: Observable<boolean>
    combinedStream$: Subscription
    focusAllPinsSub$: Subscription
    showEventsSub$: Subscription
    showUserChat$: Observable<boolean>;
    showAllUserChat$: Observable<boolean>;

    defaultOptions: MapDefaults = {
        mapId: 'defaultMap',
        useHtmlLayer: true,
        zoom: environment.mapDefaults.zoom,
        showZoomControls: true,
    }

    pins: MapPin[] = []

    constructor (private store: Store<AppState>, private updates$: Actions, private directlineService: DirectlineService) { }

    ngOnInit() {
        const responseStream = this.store.pipe(select(responsesSelector));
        const eventStream = this.store.pipe(select(eventsSelector));
        const deviceStream = this.store.pipe(select(devicesSelector));

        this.showUserChat$ = this.store.pipe(select(chatShowUserChatSelector));
        this.showAllUserChat$ = this.store.pipe(select(chatShowAllChatSelector));

        this.store
            .pipe(
                select(chatAuthSelector),
                filter(auth => auth.token !== null),
                first(auth => auth.token !== null))
            .subscribe(auth => {
                this.directlineService.connect(auth.token, auth.user).subscribe(chatMessage => {
                    this.store.dispatch(new AddChat({ chat: chatMessage }))
                });
            });

        this.combinedStream$ = combineLatest(
            responseStream,
            eventStream,
            deviceStream
        ).subscribe(([ responses, events, devices ]) =>
            this.updatePins(devices, events, responses)
        )

        this.showEventsSub$ = this.store
            .pipe(select(showEventsSelector))
            .subscribe(events => {
                if (this.pins.length > 0 && events && events.length > 0) {
                    this.mapComponent.focusEvents(events)
                }
            })

        this.focusAllPinsSub$ = this.updates$
            .ofType(AppActionTypes.FocusAllPins)
            .subscribe(() => {
                this.mapComponent.focusAllPins()
            })

        this.showResponses$ = this.store.pipe(select(responsesExist))

        this.store.dispatch(new GetDevices())
        this.store.dispatch(new GetEvents())
        this.store.dispatch(new GetResponses())
        this.store.dispatch(new UpdatePageTitle({ title: 'Right Now' }))
        this.store.dispatch(new GetChatAuthToken())
    }

    ngOnDestroy() {
        this.combinedStream$.unsubscribe()
        this.focusAllPinsSub$.unsubscribe()
        this.showEventsSub$.unsubscribe()
    }

    getPinColor(event: Event, response: Response) {
        if (response) {
            if (response.responseState === ResponseState.Active) {
                return response.color // response is active
            } else {
                return 'green' // response has been resolved
            }
        } else {
            if (event.closureDate === null) {
                return 'blue'
            } else {
                return 'grey'
            }
        }
    }

    updatePins(devices: Device[], events: Event[], responses: Response[]) {
        const devicePins = devices.map(device => {
            const pin: MapPin = { ...device }
            const recentEvent = this.getRecentEvent(events, device.deviceId)

            if (recentEvent) {
                const activeResponse = this.getActiveResponse(
                    responses,
                    recentEvent.eventClusterId
                )

                pin.event = recentEvent
                pin.color = this.getPinColor(recentEvent, activeResponse)
            }

            return pin
        })

        const eventPins = events
            .filter(e => e.eventType === EventType.Message)
            .map(event => ({
                ...event.device,
                event: event,
                color: this.getPinColor(event, this.getActiveResponse(responses, event.eventClusterId))
            }))

        this.pins = [ ...devicePins, ...eventPins ];
    }

    getActiveResponse(responses: Response[], eventClusterId: string) {
        return responses.find(
            response =>
                response.primaryEventClusterId === eventClusterId ||
                response.eventClusterIds.some(eci => eci === eventClusterId)
        )
    }

    getRecentEvent(events: Event[], deviceId) {
        if (events.length <= 0) {
            return null
        }

        return events
            .filter(event => event.device.deviceId === deviceId)
            .sort((a, b) => b.startDate - a.startDate)
            .slice(0, 1)[ 0 ]
    }
}
