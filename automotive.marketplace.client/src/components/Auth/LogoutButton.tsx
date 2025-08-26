import { useAppDispatch } from "@/shared/hooks/redux";
import { clearCredentials } from "@/shared/state/authSlice";
import { useLogoutUser } from "@/shared/utils/queries/auth/useLogoutUser";
import { Button } from "../ui/button";

const LogoutButton = () => {
  const dispatch = useAppDispatch();
  const { mutateAsync: logoutUserAsync } = useLogoutUser();

  const handleLogout = async () => {
    await logoutUserAsync();
    dispatch(clearCredentials());
  };

  return (
    <Button variant="secondary" onClick={() => void handleLogout()}>
      Logout
    </Button>
  );
};

export default LogoutButton;
