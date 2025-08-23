import { useLogoutAccount } from "@/shared/utils/queries/auth/useLogoutAccount";
import { Button } from "../ui/button";
import { useAppDispatch } from "@/shared/hooks/redux";
import { clearCredentials } from "@/shared/state/authSlice";

const LogoutButton = () => {
  const dispatch = useAppDispatch();
  const { mutateAsync: logoutAsync } = useLogoutAccount();

  const handleLogout = async () => {
    await logoutAsync();
    dispatch(clearCredentials());
  };

  return (
    <Button variant="secondary" onClick={async () => handleLogout()}>
      Logout
    </Button>
  );
};

export default LogoutButton;
