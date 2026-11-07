# Account avatar and host grid integration findings

Status: implemented and verified locally on 2026-11-06. See the accompanying change record for test evidence and limitations.

## Symptoms and diagnosis

Colligere account Home and Services could both appear current. PrimaryNavigationItem has a reviewed Prefix default and an internal NavLink. The host supplied IsCurrent=false on Home while leaving Prefix matching active; automatic matching still correctly selected the /account/ prefix. The host must set Match=All for this landing route. The Gallery's navigation sample already demonstrates exact matching. This is a host integration issue, not a reason to change every consumer's matching default.

Account quick actions appeared tall. UniformGrid defaults to rectangular 2:1 cells and MaxCellHeight=260; the maximum is not the default desired compact height. Existing fixed-height CellHeight and icon-support configuration provide compact actions. Pages that render a grid directly under PageHeading have only the grid's margin; Section, even without Title, supplies the same standard top padding used by pages with a subtitle. These are composition choices owned by the application.

Flourish lacked an account-avatar control. ImagePreview is a figure/media preview and cannot provide account initials, circular sizing and fallback semantics. Avatar now implements those capabilities once in Framework, with Design-only color treatment and a real Gallery UG composition.

## Verification and limits

Avatar tests cover encoded names, Unicode graphemes, icon fallback, decorative semantics, image URL validation, parameter boundaries, error fallback, stale source events, retry and information-grid composition. Framework 531 checks, Gallery 9,254, three-language 26,045, ChangeLog 216, CSS bundle 21 and fresh local-package consumers 155 passed; Release builds are warning/error free. These checks do not assert physical browser appearance or image-network behavior. Follow the manual checklist in the change record. No global navigation/grid behavior changed, and no public release or runtime deployment occurred.
