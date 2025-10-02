import { PERMISSIONS } from "@/constants/permissions";
import { LogoutButton, RegisterButton } from "@/features/auth";
import { useAppSelector } from "@/hooks/redux";
import { Link } from "@tanstack/react-router";
import { Button } from "../../ui/button";
import ThemeToggle from "./ThemeToggle";

const Header = () => {
  const { userId, permissions } = useAppSelector((state) => state.auth);

  return (
    <div className="dark:bg-secondary shadow-lg/2 dark:shadow-lg/4 dark:shadow-rose-600">
      <div className="mx-8 flex items-center justify-between py-2 xl:mx-auto xl:max-w-6xl">
        <div className="space-x-2 truncate">
          <Link to="/" className="[&.active]:font-bold">
            Automotive Marketplace
          </Link>{" "}
        </div>
        <div className="flex items-center space-x-2 truncate">
          {permissions.includes(PERMISSIONS.ViewModels) && (
            <Link to="/models">
              <Button variant="link">Models</Button>
            </Link>
          )}
          {permissions.includes(PERMISSIONS.ViewCars) && (
            <Link to="/cars">
              <Button variant="link">Cars</Button>
            </Link>
          )}
          <Link to="/listing/create">
            <Button variant="link">Sell your car</Button>
          </Link>
          <ThemeToggle />
          {userId ? <LogoutButton /> : <RegisterButton />}
        </div>
      </div>
    </div>
  );
};

export default Header;
