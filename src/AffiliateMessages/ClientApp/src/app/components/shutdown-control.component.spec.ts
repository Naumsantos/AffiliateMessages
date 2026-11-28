import { TestBed } from '@angular/core/testing';
import { Subject, throwError } from 'rxjs';
import { ConversionApiService } from '../services/conversion-api.service';
import { ShutdownControlComponent } from './shutdown-control.component';

describe('ShutdownControlComponent', () => {
  let api: { shutdownHost: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    api = { shutdownHost: vi.fn() };
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    await TestBed.configureTestingModule({
      imports: [ShutdownControlComponent],
      providers: [{ provide: ConversionApiService, useValue: api }],
    }).compileComponents();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('does not request shutdown when the user cancels confirmation', () => {
    vi.mocked(window.confirm).mockReturnValue(false);
    const component = TestBed.createComponent(ShutdownControlComponent).componentInstance;

    component.requestShutdown();

    expect(api.shutdownHost).not.toHaveBeenCalled();
    expect(component.isShuttingDown()).toBe(false);
  });

  it('keeps the control disabled after the host accepts shutdown', () => {
    const pending = new Subject<void>();
    api.shutdownHost.mockReturnValue(pending);
    const fixture = TestBed.createComponent(ShutdownControlComponent);

    fixture.componentInstance.requestShutdown();
    expect(fixture.componentInstance.isShuttingDown()).toBe(true);

    pending.next();
    pending.complete();

    expect(fixture.componentInstance.isShuttingDown()).toBe(true);
  });

  it('restores the control and reports a failed shutdown request', () => {
    api.shutdownHost.mockReturnValue(throwError(() => new Error('offline')));
    const component = TestBed.createComponent(ShutdownControlComponent).componentInstance;

    component.requestShutdown();

    expect(component.isShuttingDown()).toBe(false);
    expect(component.shutdownError()).toBeTruthy();
  });
});
