import { useLogoutAccount } from "@/shared/utils/queries/auth/useLogoutAccount";
import { Button } from "../ui/button";

const LogoutButton = () => {
  const { mutateAsync: logoutAsync } = useLogoutAccount();

  return (
    <Button variant="secondary" onClick={async () => await logoutAsync()}>
      Logout
    </Button>
  );
};

export default LogoutButton;
