
// fdescribe('StatisticsCollector', () => {
//   const mockaAuthData = {
//     isAuthenticated: true,
//     accessToken: "",
//     refreshToken: "",
//     refreshTokenExpiryDate: new Date()
//   };

//   let service: StatisticsCollector;
//   let mockErrorHandler: jasmine.SpyObj<CustomErrorHandler>;
//   let mockAuthService: jasmine.SpyObj<AuthenticationService>;
//   let mockHubConnection: jasmine.SpyObj<signalR.HubConnection>;
//   let authDataSubject$: BehaviorSubject<AuthData> = new BehaviorSubject<AuthData>(mockaAuthData);

//   beforeEach(() => {
//     mockErrorHandler = jasmine.createSpyObj('CustomErrorHandler', ['handleHubError']);
//     mockAuthService = jasmine.createSpyObj('AuthenticationService', ['getAuthData']);
//     authDataSubject$ = new BehaviorSubject<AuthData>(mockaAuthData);
//     mockAuthService.getAuthData.and.returnValue(authDataSubject$.asObservable());

//     mockHubConnection = jasmine.createSpyObj('HubConnection', ['start', 'on', 'invoke', 'state']);

//     TestBed.configureTestingModule({
//       providers: [
//         StatisticsCollector,
//         { provide: CustomErrorHandler, useValue: mockErrorHandler },
//         { provide: AuthenticationService, useValue: mockAuthService },
//       ]
//     });

//     service = TestBed.inject(StatisticsCollector);
//   });

//   afterEach(() => {
//     authDataSubject$.complete();
//   });

//   it('should update auth token and delete old connections when auth data changes', () => {
//     spyOn(service as any, 'deleteOldConnections');

//     const newToken = 'new-token';
//     authDataSubject$.next({
//       isAuthenticated: true,
//       accessToken: newToken,
//       refreshToken: "",
//       refreshTokenExpiryDate: new Date()
//     });

//     expect(service['authToken']).toBe(newToken);
//     expect(service['deleteOldConnections']).toHaveBeenCalled();
//   });

//   it('should start a new connection if not already connected', (done) => {
//     Object.defineProperty(mockHubConnection, 'state', {
//       get: () => HubConnectionState.Disconnected,
//       configurable: true,
//       enumerable: true
//     });
//     service["authToken"] = "token";

//     spyOn(service as any, 'createNewHubConnection').and.returnValue(mockHubConnection);
//     mockHubConnection.start.and.returnValue(Promise.resolve());

//     service.startConnection('test-url').subscribe({
//       complete: () => {
//         expect(mockHubConnection.start).toHaveBeenCalled();
//         done();
//       }
//     });

//   });

//   it('should receive statistics from the hub', (done) => {
//     const mockData = { key: 'test-key', data: 'some-data' };

//     mockHubConnection.on.and.callFake((method: string, callback: Function) => {
//       if (method === 'ReceiveStatistics') {
//         callback(mockData.key, mockData.data);
//       }
//     });

//     spyOn(service as any, 'getHubConnection').and.returnValue(mockHubConnection);

//     service.receiveStatistics('test-url').subscribe((result) => {
//       expect(result).toEqual(mockData);
//       done();
//     });
//   });

//   it('should handle errors when starting the connection', async () => {
//     Object.defineProperty(mockHubConnection, 'state', {
//       get: () => HubConnectionState.Disconnected,
//       configurable: true,
//       enumerable: true
//     });

//     const error = new Error('Connection failed');
//     mockHubConnection.start.and.returnValue(Promise.reject(error));

//     spyOn(service as any, 'createNewHubConnection').and.returnValue(mockHubConnection);

//     await service.startConnection('test-url').toPromise().catch(err => {
//       expect(err).toBe(error);
//       expect(mockErrorHandler.handleHubError).toHaveBeenCalledWith(error);
//     });
//   });

//   it('should clean up on destroy', () => {
//     spyOn(service['destroy$'], 'next').and.callThrough();
//     spyOn(service['destroy$'], 'complete').and.callThrough();

//     service.ngOnDestroy();

//     expect(service['destroy$'].next).toHaveBeenCalled();
//     expect(service['destroy$'].complete).toHaveBeenCalled();
//   });

//   it('should delete old connections that are disconnected', () => {
//     Object.defineProperty(mockHubConnection, 'state', {
//       get: () => HubConnectionState.Disconnected,
//       configurable: true,
//       enumerable: true
//     });
//     service['hubConnections'].set('test-url', { connection: mockHubConnection, promise: Promise.resolve() });

//     service['deleteOldConnections']();

//     expect(service['hubConnections'].size).toBe(0);
//   });

//   it('should invoke StartListen on the hub connection and handle errors', fakeAsync(() => {
//     const hubUrl = 'test-url';
//     const key = 'test-key';
//     const error = new Error('Invoke failed');

//     spyOn(service as any, 'getHubConnection').and.returnValue(mockHubConnection);
//     mockHubConnection.invoke.and.returnValue(Promise.reject(error));

//     service.startListen(hubUrl, key);
//     flush()

//     expect(mockHubConnection.invoke).toHaveBeenCalledWith('StartListen', key);
//     expect(mockHubConnection.invoke).toHaveBeenCalledTimes(1);
//     expect(mockErrorHandler.handleHubError).toHaveBeenCalledWith(error);
//   }));
// }); 