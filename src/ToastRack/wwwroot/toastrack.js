// ToastRack JS interop module. Loaded lazily by the ToastRack component via
// import('./_content/ToastRack/toastrack.js').

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
