// ToastRack JS interop module. Loaded lazily by the ToastRackHost component via
// import('./_content/ToastRack/toastrack.js').

const outsideClickHandlers = new Map();
let boundaryState = null;

/**
 * Scrolls the element with the given id to its bottom.
 */
export function scrollToBottom(elementId) {
  const el = document.getElementById(elementId);
  if (el) {
    el.scrollTop = el.scrollHeight;
  }
}

/**
 * Invokes the given .NET method whenever a click lands outside the element with the given id.
 */
export function registerOutsideClick(elementId, dotNetRef, methodName) {
  unregisterOutsideClick(elementId);
  const handler = (event) => {
    const el = document.getElementById(elementId);
    if (el && !el.contains(event.target)) {
      dotNetRef.invokeMethodAsync(methodName);
    }
  };
  document.addEventListener('click', handler);
  outsideClickHandlers.set(elementId, handler);
}

/**
 * Removes an outside-click handler previously registered for the element.
 */
export function unregisterOutsideClick(elementId) {
  const handler = outsideClickHandlers.get(elementId);
  if (handler) {
    document.removeEventListener('click', handler);
    outsideClickHandlers.delete(elementId);
  }
}

/**
 * Observes the first element matching the selector and reports its bounding rectangle
 * (viewport coordinates) to .NET on every resize. Returns false when no element matches.
 */
export function observeBoundary(selector, dotNetRef, methodName) {
  unobserveBoundary();

  const el = document.querySelector(selector);
  if (!el) {
    return false;
  }

  const report = () => {
    const rect = el.getBoundingClientRect();
    dotNetRef.invokeMethodAsync(methodName, {
      left: rect.left,
      top: rect.top,
      width: rect.width,
      height: rect.height,
    });
  };

  const observer = new ResizeObserver(report);
  observer.observe(el);
  window.addEventListener('resize', report);
  boundaryState = { observer, report };

  report();
  return true;
}

/**
 * Stops observing the current boundary element, if any.
 */
export function unobserveBoundary() {
  if (boundaryState) {
    boundaryState.observer.disconnect();
    window.removeEventListener('resize', boundaryState.report);
    boundaryState = null;
  }
}
