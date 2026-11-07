# Form validation

Shared textbox, number, textarea, select, datepicker, checkbox and image upload
components read their own `NgControl`. A field shows a red border and a Thai
message below it when invalid and touched. Submit handlers call
`markAllAsTouched()` before returning without sending an API request. Untouched
forms remain quiet; corrections, disabling and resets update the display.

Messages cover required values, email, min/max length, numeric min/max, pattern
and custom errors. Existing explicit `errorMessage` inputs retain precedence.
Inputs link to their error using `aria-describedby` and expose `aria-invalid`.
Required indicators also follow validators configured on the Reactive Form.

The shared field subscribes after view initialization so `FormControlName` has
registered its control. Control events request change detection, including when
submit handlers or asynchronous code update forms in a zoneless application.

Reviewed forms: login; inventory entry, costs, price and status; purchase wizard
and payments; POS customer and payment; sale cancellation; serial trace; master
data brands, categories, grades, products and customers; standalone customer and
product dialogs; store settings, user creation and roles; Super Admin store
creation; sticker layout and printing settings.

The sticker editor uses Reactive Forms for its name and numeric settings while
keeping signal-driven previews. Invalid values remain visible in their inputs;
preview calculations use safe defaults. Hidden paper dimensions and code settings
are excluded from validation. Width constraints appear under the paper field.

API errors without a field identifier continue to use the existing notification
and page feedback; this change does not guess which field caused a business error.

Checks:

```powershell
npm.cmd test -- --watch=false
npm.cmd run build -- --configuration development
node scripts/check-table-browser.cjs validation
npm.cmd run build
```

The browser check uses a local mock API at desktop and mobile widths. Screenshots
and results are written under `.angular/table-ui-check/`. Mutation requests never
reach a real shop database. Angular debug APIs require the development build.
