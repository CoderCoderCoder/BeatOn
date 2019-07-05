var autoScroll = (function() {
  "use strict";
  function e(e, n) {
    return "function" ==
      typeof (e = (function(e, t) {
        return void 0 === e ? (void 0 === n ? e : n) : e;
      })(e))
      ? function() {
          for (
            var n = arguments, t = arguments.length, o = Array(t), r = 0;
            r < t;
            r++
          )
            o[r] = n[r];
          return !!e.apply(this, o);
        }
      : e
      ? function() {
          return !0;
        }
      : function() {
          return !1;
        };
  }
  function n(e, n) {
    if (((n = u(n, !0)), !g(n))) return -1;
    for (var t = 0; t < e.length; t++) if (e[t] === n) return t;
    return -1;
  }
  function t(e, t) {
    return -1 !== n(e, t);
  }
  function o(e, n) {
    for (var o = 0; o < n.length; o++) t(e, n[o]) || e.push(n[o]);
    return n;
  }
  function r(e) {
    for (var n = arguments, t = [], r = arguments.length - 1; r-- > 0; )
      t[r] = n[r + 1];
    return o(e, (t = t.map(u)));
  }
  function i(e) {
    for (var t = arguments, o = [], r = arguments.length - 1; r-- > 0; )
      o[r] = t[r + 1];
    return o.map(u).reduce(function(t, o) {
      var r = n(e, o);
      return -1 !== r ? t.concat(e.splice(r, 1)) : t;
    }, []);
  }
  function u(e, n) {
    if ("string" == typeof e)
      try {
        return document.querySelector(e);
      } catch (e) {
        throw e;
      }
    if (!g(e) && !n) throw new TypeError(e + " is not a DOM element.");
    return e;
  }
  function a(e) {
    if (e === window)
      return (function() {
        var e = {
          top: { value: 0, enumerable: !0 },
          left: { value: 0, enumerable: !0 },
          right: { value: window.innerWidth, enumerable: !0 },
          bottom: { value: window.innerHeight, enumerable: !0 },
          width: { value: window.innerWidth, enumerable: !0 },
          height: { value: window.innerHeight, enumerable: !0 },
          x: { value: 0, enumerable: !0 },
          y: { value: 0, enumerable: !0 }
        };
        if (Object.create) return Object.create({}, e);
        var n = {};
        return Object.defineProperties(n, e), n;
      })();
    try {
      var n = e.getBoundingClientRect();
      return void 0 === n.x && ((n.x = n.left), (n.y = n.top)), n;
    } catch (n) {
      throw new TypeError("Can't call getBoundingClientRect on " + e);
    }
  }
  function c(e) {
    function n(e) {
      for (var n = 0; n < y.length; n++) t[y[n]] = e[y[n]];
    }
    var t = {
      screenX: 0,
      screenY: 0,
      clientX: 0,
      clientY: 0,
      ctrlKey: !1,
      shiftKey: !1,
      altKey: !1,
      metaKey: !1,
      button: 0,
      buttons: 1,
      relatedTarget: null,
      region: null
    };
    return (
      void 0 !== e && e.addEventListener("mousemove", n),
      {
        destroy: function() {
          e && e.removeEventListener("mousemove", n, !1), (t = null);
        },
        dispatch: MouseEvent
          ? function(e, n, o) {
              var r = new MouseEvent("mousemove", l(t, n));
              return d(r, o), e.dispatchEvent(r);
            }
          : "function" == typeof document.createEvent
          ? function(e, n, o) {
              var r = l(t, n),
                i = document.createEvent("MouseEvents");
              return (
                i.initMouseEvent(
                  "mousemove",
                  !0,
                  !0,
                  window,
                  0,
                  r.screenX,
                  r.screenY,
                  r.clientX,
                  r.clientY,
                  r.ctrlKey,
                  r.altKey,
                  r.shiftKey,
                  r.metaKey,
                  r.button,
                  r.relatedTarget
                ),
                d(i, o),
                e.dispatchEvent(i)
              );
            }
          : "function" == typeof document.createEventObject
          ? function(e, n, o) {
              var r = document.createEventObject(),
                i = l(t, n);
              for (var u in i) r[u] = i[u];
              return d(r, o), e.dispatchEvent(r);
            }
          : void 0
      }
    );
  }
  function l(e, n) {
    n = n || {};
    for (var t = h(e), o = 0; o < y.length; o++)
      void 0 !== n[y[o]] && (t[y[o]] = n[y[o]]);
    return t;
  }
  function d(e, n) {
    console.log("data ", n), (e.data = n || {}), (e.dispatched = "mousemove");
  }
  function f(n, o) {
    function u(e) {
      for (var t = 0; t < n.length; t++)
        if (n[t] === e.target) {
          Y = !0;
          break;
        }
      Y &&
        v(function() {
          return (Y = !1);
        });
    }
    function l() {
      A = !0;
    }
    function d() {
      (A = !1), f();
    }
    function f() {
      w(S), w(M);
    }
    function m() {
      A = !1;
    }
    function p(e) {
      if (!e) return null;
      if (F === e) return e;
      if (t(n, e)) return e;
      for (; (e = e.parentNode); ) if (t(n, e)) return e;
      return null;
    }
    function g() {
      for (var e = null, t = 0; t < n.length; t++) s(x, n[t]) && (e = n[t]);
      return e;
    }
    function h(e) {
      if (L.autoScroll() && !e.dispatched) {
        var n = e.target,
          t = document.body;
        F && !s(x, F) && (L.scrollWhenOutside || (F = null)),
          n && n.parentNode === t ? (n = g()) : (n = p(n)) || (n = g()),
          n && n !== F && (F = n),
          j && (w(M), (M = v(y))),
          F && (w(S), (S = v(b)));
      }
    }
    function y() {
      E(j), w(M), (M = v(y));
    }
    function b() {
      F && (E(F), w(S), (S = v(b)));
    }
    function E(e) {
      var n,
        t,
        o = a(e);
      (n =
        x.x < o.left + L.margin
          ? Math.floor(Math.max(-1, (x.x - o.left) / L.margin - 1) * L.maxSpeed)
          : x.x > o.right - L.margin
          ? Math.ceil(Math.min(1, (x.x - o.right) / L.margin + 1) * L.maxSpeed)
          : 0),
        (t =
          x.y < o.top + L.margin
            ? Math.floor(
                Math.max(-1, (x.y - o.top) / L.margin - 1) * L.maxSpeed
              )
            : x.y > o.bottom - L.margin
            ? Math.ceil(
                Math.min(1, (x.y - o.bottom) / L.margin + 1) * L.maxSpeed
              )
            : 0),
        L.syncMove() &&
          O.dispatch(e, {
            pageX: x.pageX + n,
            pageY: x.pageY + t,
            clientX: x.x + n,
            clientY: x.y + t
          }),
        setTimeout(function() {
          t &&
            (function(e, n) {
              e === window
                ? window.scrollTo(e.pageXOffset, e.pageYOffset + n)
                : (e.scrollTop += n);
            })(e, t),
            n &&
              (function(e, n) {
                e === window
                  ? window.scrollTo(e.pageXOffset + n, e.pageYOffset)
                  : (e.scrollLeft += n);
              })(e, n);
        });
    }
    void 0 === o && (o = {});
    var L = this,
      X = 4,
      Y = !1;
    (this.margin = o.margin || -1),
      (this.scrollWhenOutside = o.scrollWhenOutside || !1);
    var x = {},
      T = (function(n, t) {
        var o = e((t = t || {}).allowUpdate, !0);
        return function(e) {
          if (
            ((e = e || window.event),
            (n.target = e.target || e.srcElement || e.originalTarget),
            (n.element = this),
            (n.type = e.type),
            o(e))
          ) {
            if (e.targetTouches)
              (n.x = e.targetTouches[0].clientX),
                (n.y = e.targetTouches[0].clientY),
                (n.pageX = e.targetTouches[0].pageX),
                (n.pageY = e.targetTouches[0].pageY),
                (n.screenX = e.targetTouches[0].screenX),
                (n.screenY = e.targetTouches[0].screenY);
            else {
              if (null === e.pageX && null !== e.clientX) {
                var t = (e.target && e.target.ownerDocument) || document,
                  r = t.documentElement,
                  i = t.body;
                (n.pageX =
                  e.clientX +
                  ((r && r.scrollLeft) || (i && i.scrollLeft) || 0) -
                  ((r && r.clientLeft) || (i && i.clientLeft) || 0)),
                  (n.pageY =
                    e.clientY +
                    ((r && r.scrollTop) || (i && i.scrollTop) || 0) -
                    ((r && r.clientTop) || (i && i.clientTop) || 0));
              } else (n.pageX = e.pageX), (n.pageY = e.pageY);
              (n.x = e.clientX),
                (n.y = e.clientY),
                (n.screenX = e.screenX),
                (n.screenY = e.screenY);
            }
            (n.clientX = n.x), (n.clientY = n.y);
          }
        };
      })(x),
      O = c(),
      A = !1;
    window.addEventListener("mousemove", T, !1),
      window.addEventListener("touchmove", T, !1),
      isNaN(o.maxSpeed) || (X = o.maxSpeed),
      (this.autoScroll = e(o.autoScroll)),
      (this.syncMove = e(o.syncMove, !1)),
      (this.destroy = function(e) {
        window.removeEventListener("mousemove", T, !1),
          window.removeEventListener("touchmove", T, !1),
          window.removeEventListener("mousedown", l, !1),
          window.removeEventListener("touchstart", l, !1),
          window.removeEventListener("mouseup", d, !1),
          window.removeEventListener("touchend", d, !1),
          window.removeEventListener("pointerup", d, !1),
          window.removeEventListener("mouseleave", m, !1),
          window.removeEventListener("mousemove", h, !1),
          window.removeEventListener("touchmove", h, !1),
          window.removeEventListener("scroll", u, !0),
          (n = []),
          e && f();
      }),
      (this.add = function() {
        for (var e = [], t = arguments.length; t--; ) e[t] = arguments[t];
        return r.apply(void 0, [n].concat(e)), this;
      }),
      (this.remove = function() {
        for (var e = [], t = arguments.length; t--; ) e[t] = arguments[t];
        return i.apply(void 0, [n].concat(e));
      });
    var M,
      j = null;
    "[object Array]" !== Object.prototype.toString.call(n) && (n = [n]),
      (function(e) {
        (n = []),
          e.forEach(function(e) {
            e === window ? (j = window) : L.add(e);
          });
      })(n),
      Object.defineProperties(this, {
        down: {
          get: function() {
            return A;
          }
        },
        maxSpeed: {
          get: function() {
            return X;
          }
        },
        point: {
          get: function() {
            return x;
          }
        },
        scrolling: {
          get: function() {
            return Y;
          }
        }
      });
    var S,
      F = null;
    window.addEventListener("mousedown", l, !1),
      window.addEventListener("touchstart", l, !1),
      window.addEventListener("mouseup", d, !1),
      window.addEventListener("touchend", d, !1),
      window.addEventListener("pointerup", d, !1),
      window.addEventListener("mousemove", h, !1),
      window.addEventListener("touchmove", h, !1),
      window.addEventListener("mouseleave", m, !1),
      window.addEventListener("scroll", u, !0);
  }
  function s(e, n, t) {
    return t
      ? e.y > t.top && e.y < t.bottom && e.x > t.left && e.x < t.right
      : (function(e, n) {
          var t = a(n);
          return e.y > t.top && e.y < t.bottom && e.x > t.left && e.x < t.right;
        })(e, n);
  }
  var m = ["webkit", "moz", "ms", "o"],
    v = (function() {
      for (var e = 0, n = m.length; e < n && !window.requestAnimationFrame; ++e)
        window.requestAnimationFrame = window[m[e] + "RequestAnimationFrame"];
      return (
        window.requestAnimationFrame ||
          (function() {
            var e = 0;
            window.requestAnimationFrame = function(n) {
              var t = new Date().getTime(),
                o = Math.max(0, 16 - t - e),
                r = window.setTimeout(function() {
                  return n(t + o);
                }, o);
              return (e = t + o), r;
            };
          })(),
        window.requestAnimationFrame.bind(window)
      );
    })(),
    w = (function() {
      for (var e = 0, n = m.length; e < n && !window.cancelAnimationFrame; ++e)
        window.cancelAnimationFrame =
          window[m[e] + "CancelAnimationFrame"] ||
          window[m[e] + "CancelRequestAnimationFrame"];
      return (
        window.cancelAnimationFrame ||
          (window.cancelAnimationFrame = function(e) {
            window.clearTimeout(e);
          }),
        window.cancelAnimationFrame.bind(window)
      );
    })(),
    p =
      (Math.pow(2, 53),
      "function" == typeof Array.from && Array,
      Array,
      Object,
      "function" == typeof Symbol && "symbol" == typeof Symbol.iterator
        ? function(e) {
            return typeof e;
          }
        : function(e) {
            return e && "function" == typeof Symbol && e.constructor === Symbol
              ? "symbol"
              : typeof e;
          }),
    g = function(e) {
      return (
        null != e &&
        "object" === (void 0 === e ? "undefined" : p(e)) &&
        1 === e.nodeType &&
        "object" === p(e.style) &&
        "object" === p(e.ownerDocument)
      );
    },
    h =
      "function" != typeof Object.create
        ? (function(e) {
            var n = function() {};
            return function(e, t) {
              if (e !== Object(e) && null !== e)
                throw TypeError("Argument must be an object, or null");
              n.prototype = e || {};
              var o = new n();
              return (
                (n.prototype = null),
                void 0 !== t && Object.defineProperties(o, t),
                null === e && (o.__proto__ = null),
                o
              );
            };
          })()
        : Object.create,
    y = [
      "altKey",
      "button",
      "buttons",
      "clientX",
      "clientY",
      "ctrlKey",
      "metaKey",
      "movementX",
      "movementY",
      "offsetX",
      "offsetY",
      "pageX",
      "pageY",
      "region",
      "relatedTarget",
      "screenX",
      "screenY",
      "shiftKey",
      "which",
      "x",
      "y"
    ];
  return function(e, n) {
    return new f(e, n);
  };
})();
