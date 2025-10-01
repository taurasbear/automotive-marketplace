import { LogoutButton } from "@/features/auth";
import { Link } from "@tanstack/react-router";
import { Button } from "../../ui/button";
import ThemeToggle from "./ThemeToggle";

const Header = () => {
  return (
    <div className="dark:bg-secondary shadow-lg/2 dark:shadow-lg/4 dark:shadow-rose-600">
      <div className="mx-8 flex items-center justify-between py-2 xl:mx-auto xl:max-w-6xl">
        <div className="space-x-2 truncate">
          <Link to="/" className="[&.active]:font-bold">
            Automotive Marketplace
          </Link>{" "}
        </div>
        <div className="flex items-center space-x-2 truncate">
          <Link to="/listing/create">
            <Button variant="link">Sell your car</Button>
          </Link>
          <ThemeToggle />
          <LogoutButton />
        </div>
      </div>
    </div>
  );
};

export default Header;
