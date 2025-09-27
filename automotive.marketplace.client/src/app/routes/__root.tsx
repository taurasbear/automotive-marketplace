import { LogoutButton } from "@/features/auth";
import { QueryClient } from "@tanstack/react-query";
import {
  createRootRouteWithContext,
  Link,
  Outlet,
} from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

interface RouterContext {
  queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: () => (
    <>
      <div className="shadow-lg/2">
        <div className="mx-8 flex items-center justify-between py-2 xl:mx-auto xl:max-w-6xl">
          <div className="space-x-2 truncate">
            <Link to="/" className="[&.active]:font-bold">
              Home
            </Link>{" "}
            <Link to="/about" className="[&.active]:font-bold">
              About
            </Link>
            <Link to="/listing/create" className="[&.active]:font-bold">
              Create listing
            </Link>
          </div>
          <div className="">
            <LogoutButton />
          </div>
        </div>
      </div>
      {/* <hr /> */}
      <div className="mx-8 xl:mx-auto xl:max-w-6xl">
        <Outlet />
        <TanStackRouterDevtools />
      </div>
    </>
  ),
});
