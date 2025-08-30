import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  RegisterUserCommand,
  useRefreshToken,
  useRegisterUser,
} from "@/features/auth";
import { RegisterSchema } from "@/features/auth/schemas/registerSchema";
import {
  setAccessToken,
  setCredentials,
} from "@/features/auth/state/authSlice";
import { useAppDispatch } from "@/hooks/redux";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";

const Register = () => {
  const { mutateAsync: registerUserAsync } = useRegisterUser();
  const { mutateAsync: refreshAsync } = useRefreshToken();

  const dispatch = useAppDispatch();

  const form = useForm<z.infer<typeof RegisterSchema>>({
    resolver: zodResolver(RegisterSchema),
    defaultValues: {
      username: "",
      email: "",
      password: "",
    },
  });

  const handleOnRefreshToken = async () => {
    const { data: user } = await refreshAsync();
    dispatch(setAccessToken({ accessToken: user.accessToken }));
  };

  const onSubmit = async (data: z.infer<typeof RegisterSchema>) => {
    const body: RegisterUserCommand = {
      username: data.username,
      email: data.email,
      password: data.password,
    };

    const { data: user } = await registerUserAsync(body);

    dispatch(
      setCredentials({
        accessToken: user.accessToken,
        permissions: [],
        userId: user.userId,
      }),
    );
  };

  return (
    <div className="flex min-h-screen flex-col items-center justify-center">
      <h2 className="mb-6 text-2xl font-bold">Register</h2>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="w-full max-w-md space-y-6"
        >
          <FormField
            control={form.control}
            name="username"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Username</FormLabel>
                <FormControl>
                  <Input placeholder="Your username" {...field} />
                </FormControl>
                <FormDescription>
                  This will be your public display name.
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Email</FormLabel>
                <FormControl>
                  <Input
                    type="email"
                    placeholder="you@example.com"
                    {...field}
                  />
                </FormControl>
                <FormDescription>{`We'll never share your email.`}</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="password"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Password</FormLabel>
                <FormControl>
                  <Input type="password" placeholder="Password" {...field} />
                </FormControl>
                <FormDescription>At least 6 characters.</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" className="w-full">
            Register
          </Button>
        </form>
      </Form>
      <Button className="m-4" onClick={void handleOnRefreshToken}>
        Refresh token(temporary)
      </Button>
    </div>
  );
};

export default Register;
