import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  LoginSchema,
  LoginUserCommand,
  setCredentials,
  useLoginUser,
} from "@/features/auth";
import { useAppDispatch } from "@/hooks/redux";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";

const Login = () => {
  const { mutateAsync: loginUserAsync } = useLoginUser();

  const dispatch = useAppDispatch();

  const form = useForm<z.infer<typeof LoginSchema>>({
    resolver: zodResolver(LoginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const onSubmit = async (formData: z.infer<typeof LoginSchema>) => {
    const body: LoginUserCommand = {
      email: formData.email,
      password: formData.password,
    };

    const { data: user } = await loginUserAsync(body);
    dispatch(
      setCredentials({
        accessToken: user.accessToken,
        permissions: user.permissions,
        userId: user.userId,
      }),
    );
  };

  return (
    <div className="flex min-h-screen flex-col items-center justify-center">
      <h2 className="mb-6 text-2xl font-bold">Login</h2>
      <Form {...form}>
        <form
          onSubmit={void form.handleSubmit(onSubmit)}
          className="w-full max-w-md space-y-6"
        >
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
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" className="w-full">
            Login
          </Button>
        </form>
      </Form>
    </div>
  );
};

export default Login;
