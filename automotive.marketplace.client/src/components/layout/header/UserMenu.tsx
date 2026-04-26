import { PERMISSIONS } from "@/constants/permissions";
import { useLogoutUser, clearCredentials } from "@/features/auth";
import { useAppDispatch, useAppSelector } from "@/hooks/redux";
import { Link, useNavigate } from "@tanstack/react-router";
import { LogOut, Settings, User } from "lucide-react";
import { useTranslation } from "react-i18next";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "../../ui/dropdown-menu";
import { Button } from "../../ui/button";

const UserMenu = () => {
  const { t } = useTranslation("common");
  const { permissions } = useAppSelector((state) => state.auth);
  const dispatch = useAppDispatch();
  const { mutateAsync: logoutUserAsync } = useLogoutUser();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logoutUserAsync();
    dispatch(clearCredentials());
    await navigate({ to: "/login" });
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="h-8 w-8 rounded-full">
          <User className="h-4 w-4" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <DropdownMenuLabel>
          {t("common:userMenu.sectionMyListings")}
        </DropdownMenuLabel>
        <DropdownMenuItem asChild>
          <Link to="/my-listings">{t("common:userMenu.myListings")}</Link>
        </DropdownMenuItem>

        {permissions.includes(PERMISSIONS.ViewMakes) && (
          <>
            <DropdownMenuSeparator />
            <DropdownMenuLabel>
              {t("common:userMenu.sectionAdmin")}
            </DropdownMenuLabel>
            <DropdownMenuItem asChild>
              <Link to="/makes">{t("common:userMenu.makes")}</Link>
            </DropdownMenuItem>
            <DropdownMenuItem asChild>
              <Link to="/models">{t("common:userMenu.models")}</Link>
            </DropdownMenuItem>
            <DropdownMenuItem asChild>
              <Link to="/variants">{t("common:userMenu.variants")}</Link>
            </DropdownMenuItem>
          </>
        )}

        <DropdownMenuSeparator />
        <DropdownMenuLabel>
          {t("common:userMenu.sectionAccount")}
        </DropdownMenuLabel>
        <DropdownMenuItem asChild>
          <Link to="/settings">
            <Settings className="mr-2 h-4 w-4" />
            {t("common:userMenu.profileSettings")}
          </Link>
        </DropdownMenuItem>
        <DropdownMenuItem
          className="text-red-600"
          onClick={() => void handleLogout()}
        >
          <LogOut className="mr-2 h-4 w-4" />
          {t("common:userMenu.logOut")}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
};

export default UserMenu;
